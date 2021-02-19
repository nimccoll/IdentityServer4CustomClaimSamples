//===============================================================================
// Microsoft FastTrack for Azure
// IdentityServer4 Custom Claims Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4QS.Services
{
    public class ProfileService : IProfileService
    {
        private readonly TestUserStore _users;
        private readonly SqlHelper _sqlHelper;

        public ProfileService(TestUserStore users, SqlHelper sqlHelper)
        {
            _users = users ?? new TestUserStore(Config.GetUsers());
            _sqlHelper = sqlHelper ?? new SqlHelper(Config.GetRolesConnectionString());
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            TestUser user = _users.FindBySubjectId(context.Subject.GetSubjectId());
            List<Claim> claims = context.Subject.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            // Add custom claims in token here based on user properties or any other source
            // Lookup roles from legacy databases here based on Client ID and User
            // If different databases are used for different client applications use an if or select
            // to run different code based on Client ID (see commented code below)
            List<string> roles = GetUserRoles(context.Client.ClientId, user.Username);

            //if (context.Client.ClientId == "mvc")
            //{
            //    Put code here to retrieve roles for client "mvc"
            //}
            //else if (context.Client.ClientId == "client")
            //{
            //    Put code here to retrieve roles for client "client"
            //}
            //else if (context.Client.ClientId == "ro.client")
            //{
            //    Put code here to retrieve roles for client "ro.client"
            //}

            if (roles.Count > 0)
            {
                claims.Add(new Claim("roles", JsonConvert.SerializeObject(roles)));
                foreach (string role in roles)
                {
                    claims.Add(new Claim("role", role));
                }
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = _users.FindBySubjectId(sub);
            context.IsActive = user != null;
        }

        private List<string> GetUserRoles(string clientId, string userName)
        {
            // Retrieve user roles from other database
            List<string> roles = new List<string>();

            const string sqlCommand = "SELECT Role FROM [dbo].[UserRole] WHERE ClientId = @ClientId AND UserName = @UserName";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ClientId", clientId));
            parameters.Add(new SqlParameter("@UserName", userName));
            try
            {
                SqlDataReader reader = _sqlHelper.ExecuteDataReader(sqlCommand, System.Data.CommandType.Text, ref parameters);
                while (reader.Read())
                {
                    string role = reader["Role"].ToString();
                    roles.Add(role);
                }
                reader.Close();
            }
            catch (Exception)
            {
                // Implement your logging, etc. here
                throw;
            }

            return roles;
        }
    }
}
