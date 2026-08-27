/*
    Production security model template.

    Replace the placeholders outside source control before running this script:
    - $(LeaveFlowAppLogin)
    - $(LeaveFlowAppUser)

    Do not commit production usernames, passwords, or secrets.
*/

-- CREATE USER [$(LeaveFlowAppUser)] FOR LOGIN [$(LeaveFlowAppLogin)];

-- DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Roles_GetAll TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Users_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Users_GetByNormalizedEmail TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Users_UpdateLoginSuccess TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Users_RecordFailedLogin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Users_UpdatePasswordHash TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_UserRoles_GetByUserId TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LoginAttempts_Insert TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_AuditLogs_Insert TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_GetIdByUserId TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_GetIdByUserId TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_ManagerConsultants_Exists TO [$(LeaveFlowAppUser)];
--
-- Development-only bootstrap procedures must not be granted to the production application user:
-- dbo.usp_Users_Upsert
-- dbo.usp_UserRoles_Ensure
-- dbo.usp_Consultants_EnsureForUser
-- dbo.usp_Managers_EnsureForUser
-- dbo.usp_ManagerConsultants_Ensure
