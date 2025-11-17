import { Component, OnInit, ViewChild } from '@angular/core';
import { AppLog } from '../../Models/app-log.model';
import { AppLogService } from '../../Services/app-log.service';
import { formatDate } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { AuthService } from '../../Services/auth.service';

@Component({
  selector: 'app-app-log',
  standalone: false,
  templateUrl: './app-log.component.html',
  styleUrl: './app-log.component.css'
})
export class AppLogComponent implements OnInit {

  displayedColumns: string[] = ['id', 'timeStamp', 'message'];
  dataSource: MatTableDataSource<AppLog> = new MatTableDataSource<AppLog>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  startDate!: string;
  endDate!: string;
  loading = false;
  hasPermission = false;


  constructor(private logService: AppLogService, private authService: AuthService) {}

  ngOnInit(): void {
    this.hasPermission = this.authService.hasPermission('ViewLogs');

    if (!this.hasPermission) {
      return; // Do not load logs
    }
    const today = new Date();
    this.startDate = formatDate(today, 'yyyy-MM-dd', 'en-US');
    this.endDate = formatDate(today, 'yyyy-MM-dd', 'en-US');

    this.getLogs();
  }

  getLogs(): void {
    if (!this.hasPermission) return;

    this.loading = true;
    this.logService.getLogsByDateRange(this.startDate, this.endDate).subscribe({
      next: (logs) => {
        this.dataSource = new MatTableDataSource(logs);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
}
