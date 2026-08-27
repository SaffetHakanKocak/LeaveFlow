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
