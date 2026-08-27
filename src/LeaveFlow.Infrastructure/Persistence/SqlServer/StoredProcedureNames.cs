namespace LeaveFlow.Infrastructure.Persistence.SqlServer;

internal static class StoredProcedureNames
{
    internal const string RolesGetAll = "dbo.usp_Roles_GetAll";
    internal const string UsersGetById = "dbo.usp_Users_GetById";
    internal const string UsersGetByNormalizedEmail = "dbo.usp_Users_GetByNormalizedEmail";
    internal const string UsersUpdateLoginSuccess = "dbo.usp_Users_UpdateLoginSuccess";
    internal const string UsersRecordFailedLogin = "dbo.usp_Users_RecordFailedLogin";
    internal const string UsersUpdatePasswordHash = "dbo.usp_Users_UpdatePasswordHash";
    internal const string UserRolesGetByUserId = "dbo.usp_UserRoles_GetByUserId";
    internal const string LoginAttemptsInsert = "dbo.usp_LoginAttempts_Insert";
    internal const string AuditLogsInsert = "dbo.usp_AuditLogs_Insert";
    internal const string ConsultantsGetIdByUserId = "dbo.usp_Consultants_GetIdByUserId";
    internal const string ManagersGetIdByUserId = "dbo.usp_Managers_GetIdByUserId";
    internal const string ManagerConsultantsExists = "dbo.usp_ManagerConsultants_Exists";
    internal const string UsersUpsert = "dbo.usp_Users_Upsert";
    internal const string UserRolesEnsure = "dbo.usp_UserRoles_Ensure";
    internal const string ConsultantsEnsureForUser = "dbo.usp_Consultants_EnsureForUser";
    internal const string ManagersEnsureForUser = "dbo.usp_Managers_EnsureForUser";
    internal const string ManagerConsultantsEnsure = "dbo.usp_ManagerConsultants_Ensure";
    internal const string ConsultantsGetAll = "dbo.usp_Consultants_GetAll";
    internal const string ConsultantsGetById = "dbo.usp_Consultants_GetById";
    internal const string ConsultantsCreate = "dbo.usp_Consultants_Create";
    internal const string ConsultantsUpdate = "dbo.usp_Consultants_Update";
    internal const string ConsultantsSetActive = "dbo.usp_Consultants_SetActive";
    internal const string ManagersGetAll = "dbo.usp_Managers_GetAll";
    internal const string ManagersGetById = "dbo.usp_Managers_GetById";
    internal const string ManagersCreate = "dbo.usp_Managers_Create";
    internal const string ManagersUpdate = "dbo.usp_Managers_Update";
    internal const string ManagersSetActive = "dbo.usp_Managers_SetActive";
    internal const string ManagerConsultantsAssign = "dbo.usp_ManagerConsultants_Assign";
    internal const string ManagerConsultantsRemove = "dbo.usp_ManagerConsultants_Remove";
    internal const string ManagerConsultantsGetByManagerId = "dbo.usp_ManagerConsultants_GetByManagerId";
}
