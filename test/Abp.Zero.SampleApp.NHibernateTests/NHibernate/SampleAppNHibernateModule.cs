﻿using System;
using System.Data.Common;
using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.TestBase;
using Abp.Zero.NHibernate;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;

namespace Abp.Zero.SampleApp.NHibernate
{
    [DependsOn(typeof(AbpZeroNHibernateModule), typeof(SampleAppModule), typeof(AbpTestBaseModule))]
    public class SampleAppNHibernateModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.Modules.AbpNHibernate().FluentConfiguration
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(true, true, false, IocManager.Resolve<DbConnection>(), Console.Out));
                
        }
    }
}
