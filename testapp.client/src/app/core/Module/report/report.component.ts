import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { ReportService } from '../../Services/report.service';
import { Report, ReportFilter } from '../../Models/report';
import { AuthService } from '../../Services/auth.service';



@Component({
  selector: 'app-report',
  standalone: false,
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css'],
})
export class ReportComponent implements OnInit {
  displayedColumns: string[] = [
    'reportId',
    'operationId',
    'supervisorId',
    'operationDate',
    'operationStartTime',
    'operationEndTime',
    'ugst',
    'initialLevelAtUGST',
    'createdAt',
  ];

  dataSource = new MatTableDataSource<Report>();
  filter: ReportFilter = { startDate: '', endDate: '' };
  loading = false;
  dateError: string = '';
  hasPermission = false;
  showPaginator = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild('reportContainer') reportContainer!: ElementRef;
  @ViewChild('dataToExport', { static: false }) public dataToExport!: ElementRef;
  @ViewChild('tableToExport', { static: false }) tableToExport!: ElementRef;

  constructor(
    private reportService: ReportService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.hasPermission = this.authService.hasPermission('ViewReport',);
  }

  applyFilter(): void {
    this.dateError = '';

    if (!this.filter.startDate || !this.filter.endDate) {
      this.dateError = 'Please select both start and end dates';
      return;
    }

    if (this.filter.startDate > this.filter.endDate) {
      this.dateError = 'Start date cannot be after end date';
      return;
    }

    this.loading = true;
    this.reportService.filterReports(this.filter).subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource<any>(data);
        this.dataSource.paginator = this.paginator;
        this.showPaginator = data && data.length > 0; // toggle here also
        this.loading = false;
      },
      error: (err) => {
        console.error('Error filtering reports', err);
        this.showPaginator = false;
        this.loading = false;
      },
    });
  }
}
