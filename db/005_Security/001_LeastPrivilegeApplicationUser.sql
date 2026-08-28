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
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_GetAll TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_Create TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_Update TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Consultants_SetActive TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_GetIdByUserId TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_GetAll TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_Create TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_Update TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Managers_SetActive TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_ManagerConsultants_Exists TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_ManagerConsultants_Assign TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_ManagerConsultants_Remove TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_ManagerConsultants_GetByManagerId TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_GetAll TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_Create TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_Update TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_SetActive TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_HolidayDefinitions_Delete TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OfficialHolidayDefinitions_GetAll TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OfficialHolidayDefinitions_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OfficialHolidayDefinitions_Create TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OfficialHolidayDefinitions_Update TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OfficialHolidayDefinitions_Delete TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_ExistsOverlap TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_Create TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetMine TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetById TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetPendingForManager TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetPendingForAdmin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetForReview TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_GetConflicts TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_Approve TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_LeaveRequests_Reject TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_WorkforceTimeline_GetForAdmin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_WorkforceTimeline_GetForManager TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetForAdmin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetForManager TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetForConsultant TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetDetailForAdmin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetDetailForManager TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_OrganizationCalendar_GetDetailForConsultant TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Dashboard_GetForAdmin TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Dashboard_GetForManager TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Dashboard_GetForConsultant TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_ConsultantLeaveUsage TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_MonthlyLeaveActivity TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_TeamLeaveUsage TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_PeakLeaveDays TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_StatusDistribution TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_UpcomingLeaves TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_UpcomingHolidays TO [$(LeaveFlowAppUser)];
-- GRANT EXECUTE ON OBJECT::dbo.usp_Reports_RecentLeaveRequests TO [$(LeaveFlowAppUser)];
--
-- Development-only bootstrap procedures must not be granted to the production application user:
-- dbo.usp_Users_Upsert
-- dbo.usp_UserRoles_Ensure
-- dbo.usp_Consultants_EnsureForUser
-- dbo.usp_Managers_EnsureForUser
-- dbo.usp_ManagerConsultants_Ensure
