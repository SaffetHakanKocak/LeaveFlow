namespace LeaveFlow.Application.Ai;

public static class AiToolSchemas
{
    public const string EmptyObject = """
        {"type":"object","properties":{},"additionalProperties":false}
        """;

    public const string DateRange = """
        {
          "type":"object",
          "properties":{
            "startDate":{"type":"string","format":"date"},
            "endDate":{"type":"string","format":"date"}
          },
          "additionalProperties":false
        }
        """;

    public const string LeaveRequestSearch = """
        {
          "type":"object",
          "properties":{
            "status":{"type":"string","enum":["Pending","Approved","Rejected"]},
            "startDate":{"type":"string","format":"date"},
            "endDate":{"type":"string","format":"date"}
          },
          "additionalProperties":false
        }
        """;

    public const string LeaveConflict = """
        {
          "type":"object",
          "properties":{
            "leaveRequestId":{"type":"string","format":"uuid"}
          },
          "required":["leaveRequestId"],
          "additionalProperties":false
        }
        """;
}
