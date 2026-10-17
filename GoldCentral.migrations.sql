CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE TABLE "Permission" (
        "Code" character varying(100) NOT NULL,
        "Name" character varying(200) NOT NULL,
        CONSTRAINT "PK_Permission" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE TABLE "User" (
        "Id" uuid NOT NULL,
        "Username" character varying(50) NOT NULL,
        "PasswordHash" character varying(512) NOT NULL,
        "FullName" character varying(150) NOT NULL,
        "Email" character varying(254) NOT NULL,
        "PhoneNumber" character varying(20),
        "AvatarUrl" character varying(2048),
        "DateOfBirth" date,
        "Gender" character varying(30),
        "Address" character varying(500),
        "Role" character varying(20) NOT NULL DEFAULT 'user',
        "IsActive" boolean NOT NULL,
        "EmailVerified" boolean NOT NULL,
        "PhoneVerified" boolean NOT NULL,
        "LastLoginAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_User" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_User_Role" CHECK ("Role" IN ('admin', 'user'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE TABLE "RolePermission" (
        "Role" character varying(20) NOT NULL,
        "PermissionCode" character varying(100) NOT NULL,
        CONSTRAINT "PK_RolePermission" PRIMARY KEY ("Role", "PermissionCode"),
        CONSTRAINT "CK_RolePermission_Role" CHECK ("Role" IN ('admin', 'user')),
        CONSTRAINT "FK_RolePermission_Permission_PermissionCode" FOREIGN KEY ("PermissionCode") REFERENCES "Permission" ("Code") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.delete', 'Xóa tài khoản');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.list', 'Xem danh sách tài khoản');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.profile.read', 'Xem hồ sơ của mình');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.profile.update', 'Cập nhật hồ sơ của mình');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.read', 'Xem chi tiết tài khoản');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('users.update', 'Cập nhật tài khoản');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    INSERT INTO "User" ("Id", "Address", "AvatarUrl", "CreatedAt", "DateOfBirth", "Email", "EmailVerified", "FullName", "Gender", "IsActive", "LastLoginAt", "PasswordHash", "PhoneNumber", "PhoneVerified", "Role", "UpdatedAt", "Username")
    VALUES ('8f5da64b-8e27-4ba4-a8e0-11d088da7931', 'Việt Nam', NULL, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, 'admin@example.com', FALSE, 'Quản trị viên hệ thống', 'PreferNotToSay', TRUE, NULL, 'AQAAAAIAAYagAAAAEMGbm6CRE8hR33ytf+bnxKnv92rxeYIkloGWwVTkLYNvsqcxBEZQJWTql0+Tjp2JbQ==', NULL, FALSE, 'admin', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'admin');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.delete', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.list', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.profile.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.profile.update', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.update', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.profile.read', 'user');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('users.profile.update', 'user');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE INDEX "IX_RolePermission_PermissionCode" ON "RolePermission" ("PermissionCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_User_Email" ON "User" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_User_Username" ON "User" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923080254_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260923080254_InitialCreate', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE TABLE "SystemLog" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Method" character varying(16) NOT NULL,
        "Path" character varying(2048) NOT NULL,
        "StatusCode" integer NOT NULL,
        "Success" boolean NOT NULL,
        "Message" character varying(500) NOT NULL,
        "IpAddress" character varying(64),
        "TraceId" character varying(128) NOT NULL,
        "DurationMs" bigint NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_SystemLog" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE TABLE "UploadedFile" (
        "Id" uuid NOT NULL,
        "UploadedBy" uuid NOT NULL,
        "OriginalName" character varying(255) NOT NULL,
        "StoredName" character varying(64) NOT NULL,
        "Url" character varying(256) NOT NULL,
        "ContentType" character varying(100) NOT NULL,
        "Size" bigint NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_UploadedFile" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('files.upload', 'Tải file lên');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('system-logs.read', 'Xem nhật ký hệ thống');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.upload', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('system-logs.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.upload', 'user');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE INDEX "IX_SystemLog_CreatedAt" ON "SystemLog" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE INDEX "IX_SystemLog_UserId_CreatedAt" ON "SystemLog" ("UserId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE UNIQUE INDEX "IX_UploadedFile_StoredName" ON "UploadedFile" ("StoredName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    CREATE INDEX "IX_UploadedFile_UploadedBy" ON "UploadedFile" ("UploadedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923081747_UploadFilesAndSystemLogs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260923081747_UploadFilesAndSystemLogs', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "Action" character varying(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "AfterData" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "BeforeData" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "EntityId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "EntityType" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    ALTER TABLE "SystemLog" ADD "ResponseData" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260923082628_SystemLogDetails') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260923082628_SystemLogDetails', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE TABLE "Notification" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Content" character varying(4000) NOT NULL,
        "Link" character varying(2048),
        "ReadAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Notification" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE TABLE "PasswordResetToken" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(128) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "UsedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PasswordResetToken" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE TABLE "RefreshToken" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(128) NOT NULL,
        "DeviceName" character varying(200),
        "IpAddress" character varying(64),
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RefreshToken" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE TABLE "SystemSetting" (
        "Key" character varying(100) NOT NULL,
        "Value" character varying(4000) NOT NULL,
        "Description" character varying(500),
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_SystemSetting" PRIMARY KEY ("Key")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('dashboard.read', 'Xem dashboard');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('notifications.read', 'Xem thông báo');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('notifications.update', 'Cập nhật thông báo');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('permissions.manage', 'Quản lý quyền');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('roles.manage', 'Quản lý vai trò');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('settings.manage', 'Quản lý cấu hình hệ thống');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('dashboard.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('notifications.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('notifications.update', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('permissions.manage', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('roles.manage', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('settings.manage', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('notifications.read', 'user');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('notifications.update', 'user');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE INDEX "IX_Notification_UserId_ReadAt_CreatedAt" ON "Notification" ("UserId", "ReadAt", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE UNIQUE INDEX "IX_PasswordResetToken_TokenHash" ON "PasswordResetToken" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE INDEX "IX_PasswordResetToken_UserId_ExpiresAt" ON "PasswordResetToken" ("UserId", "ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE UNIQUE INDEX "IX_RefreshToken_TokenHash" ON "RefreshToken" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    CREATE INDEX "IX_RefreshToken_UserId_ExpiresAt" ON "RefreshToken" ("UserId", "ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924023821_PlatformServices') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260924023821_PlatformServices', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924025541_FileAccessAndSettings') THEN
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('files.delete', 'Xóa file');
    INSERT INTO "Permission" ("Code", "Name")
    VALUES ('files.read', 'Xem và tải file');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924025541_FileAccessAndSettings') THEN
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.delete', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.read', 'admin');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.delete', 'user');
    INSERT INTO "RolePermission" ("PermissionCode", "Role")
    VALUES ('files.read', 'user');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924025541_FileAccessAndSettings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260924025541_FileAccessAndSettings', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924034451_AuthRolesAndDefaultSettings') THEN
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.address', 'Địa chỉ đơn vị', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.email', 'Email liên hệ', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.name', 'Tên đơn vị hoặc doanh nghiệp', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.phone', 'Số điện thoại liên hệ', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.taxCode', 'Mã số thuế', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('organization.website', 'Website đơn vị', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('system.faviconUrl', 'Đường dẫn favicon hệ thống', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('system.logoUrl', 'Đường dẫn logo hệ thống', TIMESTAMPTZ '2026-09-24T00:00:00Z', '');
    INSERT INTO "SystemSetting" ("Key", "Description", "UpdatedAt", "Value")
    VALUES ('system.name', 'Tên hiển thị của hệ thống', TIMESTAMPTZ '2026-09-24T00:00:00Z', 'Base API');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260924034451_AuthRolesAndDefaultSettings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260924034451_AuthRolesAndDefaultSettings', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016073115_GoldPortalApplications') THEN
    CREATE TABLE "PortalApp" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(400) NOT NULL,
        "Category" character varying(30) NOT NULL,
        "Platform" character varying(20) NOT NULL,
        "Url" character varying(2000),
        "ImageUrl" character varying(2000),
        "IsVisible" boolean NOT NULL DEFAULT TRUE,
        "Order" integer NOT NULL,
        "Clicks" bigint NOT NULL DEFAULT 0,
        CONSTRAINT "PK_PortalApp" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_PortalApp_Category" CHECK ("Category" IN ('vaccination', 'dermatology', 'general', 'office')),
        CONSTRAINT "CK_PortalApp_Clicks" CHECK ("Clicks" >= 0),
        CONSTRAINT "CK_PortalApp_Order" CHECK ("Order" BETWEEN 0 AND 10000),
        CONSTRAINT "CK_PortalApp_Platform" CHECK ("Platform" IN ('system', 'facebook', 'zalo', 'tiktok', 'youtube'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016073115_GoldPortalApplications') THEN
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000001', 'vaccination', 'Quản lý lịch tiêm, hồ sơ khách hàng và dịch vụ tiêm chủng.', NULL, TRUE, 'Gold Tiêm chủng', 0, 'system', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000002', 'dermatology', 'Kết nối hệ thống chăm sóc da và điều trị da liễu.', NULL, TRUE, 'Gold Da liễu', 1, 'system', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000003', 'general', 'Quản lý khám chữa bệnh và chăm sóc sức khỏe toàn diện.', NULL, TRUE, 'Gold Phòng khám', 2, 'system', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000004', 'office', 'Không gian làm việc, tài liệu và phối hợp giữa các phòng ban.', NULL, TRUE, 'Gold Văn phòng', 3, 'system', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000005', 'vaccination', 'Cập nhật kiến thức vắc xin và hoạt động tại trung tâm.', NULL, TRUE, 'Tiêm chủng Gold Ninh Bình', 4, 'facebook', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000006', 'dermatology', 'Chia sẻ hành trình chăm sóc da và những câu chuyện làm đẹp.', NULL, TRUE, 'Gold Skin & Beauty', 5, 'facebook', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000007', 'general', 'Kết nối khách hàng, tư vấn và cập nhật thông tin dịch vụ.', NULL, TRUE, 'Gold Ninh Bình Official', 6, 'zalo', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000008', 'general', 'Khám phá kiến thức sức khỏe qua những video ngắn.', NULL, TRUE, 'Một phút khỏe cùng Gold', 7, 'tiktok', NULL);
    INSERT INTO "PortalApp" ("Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url")
    VALUES ('00000000-0000-0000-0000-000000000009', 'general', 'Video tư vấn chuyên gia và hoạt động trong hệ sinh thái Gold.', NULL, TRUE, 'Y tế Toàn Cầu Gold', 8, 'youtube', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016073115_GoldPortalApplications') THEN
    CREATE INDEX "IX_PortalApp_Category_Platform" ON "PortalApp" ("Category", "Platform");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016073115_GoldPortalApplications') THEN
    CREATE INDEX "IX_PortalApp_IsVisible_Order" ON "PortalApp" ("IsVisible", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016073115_GoldPortalApplications') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261016073115_GoldPortalApplications', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016081238_PortalDailyStatisticsAndFeatured') THEN
    ALTER TABLE "PortalApp" ADD "IsFeatured" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016081238_PortalDailyStatisticsAndFeatured') THEN
    CREATE TABLE "PortalAppDailyClick" (
        "AppId" uuid NOT NULL,
        "Date" date NOT NULL,
        "Clicks" bigint NOT NULL,
        CONSTRAINT "PK_PortalAppDailyClick" PRIMARY KEY ("AppId", "Date"),
        CONSTRAINT "CK_PortalAppDailyClick_Clicks" CHECK ("Clicks" > 0),
        CONSTRAINT "FK_PortalAppDailyClick_PortalApp_AppId" FOREIGN KEY ("AppId") REFERENCES "PortalApp" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016081238_PortalDailyStatisticsAndFeatured') THEN
    CREATE INDEX "IX_PortalAppDailyClick_Date" ON "PortalAppDailyClick" ("Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016081238_PortalDailyStatisticsAndFeatured') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261016081238_PortalDailyStatisticsAndFeatured', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016090551_AddWebsitePlatform') THEN
    ALTER TABLE "PortalApp" DROP CONSTRAINT "CK_PortalApp_Platform";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016090551_AddWebsitePlatform') THEN
    ALTER TABLE "PortalApp" ADD CONSTRAINT "CK_PortalApp_Platform" CHECK ("Platform" IN ('system', 'facebook', 'zalo', 'tiktok', 'youtube', 'website'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016090551_AddWebsitePlatform') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261016090551_AddWebsitePlatform', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016100007_InternalNews') THEN
    CREATE TABLE "NewsArticle" (
        "Id" uuid NOT NULL,
        "Title" character varying(160) NOT NULL,
        "Summary" character varying(400) NOT NULL,
        "Content" character varying(30000) NOT NULL,
        "ImageUrl" character varying(2000),
        "Department" character varying(30) NOT NULL,
        "IsPublished" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "PublishedAt" timestamp with time zone,
        CONSTRAINT "PK_NewsArticle" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_NewsArticle_Department" CHECK ("Department" IN ('all', 'vaccination', 'dermatology', 'general', 'office'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016100007_InternalNews') THEN
    CREATE INDEX "IX_NewsArticle_Department_IsPublished" ON "NewsArticle" ("Department", "IsPublished");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016100007_InternalNews') THEN
    CREATE INDEX "IX_NewsArticle_IsPublished_PublishedAt" ON "NewsArticle" ("IsPublished", "PublishedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261016100007_InternalNews') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261016100007_InternalNews', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261017010413_NewsArticleViewCount') THEN
    ALTER TABLE "NewsArticle" ADD "ViewCount" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261017010413_NewsArticleViewCount') THEN
    ALTER TABLE "NewsArticle" ADD CONSTRAINT "CK_NewsArticle_ViewCount" CHECK ("ViewCount" >= 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261017010413_NewsArticleViewCount') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261017010413_NewsArticleViewCount', '10.0.10');
    END IF;
END $EF$;
COMMIT;

