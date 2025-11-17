export interface Report{
reportId: number;
  operationId: number;
  supervisorId: number;
  operationDate: string;
  operationStartTime: string;
  operationEndTime: string;
  ugst: string;
  initialLevelAtUGST: number;
  createdAt: string;
}

export interface ReportFilter{
    startDate: string;
    endDate: string;
}