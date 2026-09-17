var builder = DistributedApplication.CreateBuilder(args);

// Orchestrate PostgreSQL, Redis, CMS, and Web Storefront
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var postgresDb = postgres.AddDatabase("DefaultConnection", "giftcardbuy_db");
var cmsDb = postgres.AddDatabase("CmsConnection", "giftcardbuy_cms");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var cms = builder.AddProject<Projects.GiftStore_Cms>("giftstore-cms")
    .WithReference(cmsDb)
    .WithHttpEndpoint(port: 5050, name: "cms-api");

var web = builder.AddProject<Projects.GiftStore_Web>("giftstore-web")
    .WithReference(postgresDb)
    .WithReference(redis)
    .WithReference(cms)
    .WithExternalHttpEndpoints();

builder.Build().Run();
