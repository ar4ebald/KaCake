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

        public static void ConfigureRoles(this RoleManager<IdentityRole> context)
        {
            string[] roles = { Admin };

            foreach (var role in roles)
            {
                if (!context.RoleExistsAsync(role).Result)
                {
                    context.CreateAsync(new IdentityRole()
                    {
                        Name = role
                    }).Wait();
                }
            }
        }
    }
}
