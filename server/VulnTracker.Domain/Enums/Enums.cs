namespace VulnTracker.Domain.Enums;


public enum Severity { Low, Medium, High, Critical }
public enum FindingStatus { New, Triaged, InProgress, Remediated, AcceptedRisk }
public enum FindingSource { Internal, External, Scanner }