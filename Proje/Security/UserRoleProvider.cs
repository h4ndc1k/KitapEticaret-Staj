using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Proje.Models;


namespace Proje.Security
{
    public class UserRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var context = new KUTUPHANEEntities1())
            {
                
                var user = context.Users
                                  .Where(u => u.userName == username)
                                  .Select(u => new { u.rolID })
                                  .FirstOrDefault();

                
                if (user != null)
                {
                    var roles = context.Roles
                                       .Where(r => r.rolID == user.rolID)
                                       .Select(r => r.rolName)
                                       .ToArray();

                    return roles;
                }
            }

            
            return new string[0];
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}