﻿using AzureADB2CWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureADB2CWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
