using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace KaCake
{
    public static class RoleNames
    {
        public const string Admin = "Admin";

        public static void ConfigureRoles(this ApplicationDbContext context)
        {
            string[] roles = { Admin };

            bool added = false;

            foreach (var role in roles.Except(context.Roles.Select(role => role.Name)))
            {
                context.Roles.Add(new IdentityRole(role));
                added = true;
            }

            if (added)
            {
                context.SaveChanges();
            }
        }
    }
}
