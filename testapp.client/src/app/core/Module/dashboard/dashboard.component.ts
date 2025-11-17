import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { AppLogService } from '../../Services/app-log.service';
import { AppLog } from '../../Models/app-log.model';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  username: string = '';
  dropdownOpen = false;
  showAddUserForm = false; 
  activeSection: 'reports' | 'logs' | 'users' |null = null;
  canAddUser = false;
  canViewReports = false;
  canViewLogs = false;
  canViewUser = false;
  sidebarOpen = false;

  logs: AppLog[] = [];

  constructor(private auth: AuthService, private router: Router, private appLogServce: AppLogService) { }

  ngOnInit(): void {
    const user = this.auth.getUser();
    if (user) {
      this.username = user.username;
      this.canAddUser = this.auth.hasPermission('AddUser');
      this.canViewUser = this.auth.hasPermission('AddUser');
      this.canViewReports = this.auth.hasPermission('ViewReport');
      this.canViewLogs = this.auth.hasPermission('ViewLogs');
    } else {
      this.router.navigate(['/login']); 
    }


  }
  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  toggleDropdown() {
  this.dropdownOpen = !this.dropdownOpen;
}
  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  onUserAdded() {
    this.showAddUserForm = false;
  }
  showReports() {
    this.activeSection = 'reports';
  }

  showLogs() {
    this.activeSection = 'logs';
  }

  showUsers() {
    this.activeSection = 'users';
  }

  loadAllLogs(): void {
    this.appLogServce.getAllLogs().subscribe({
      next: (data) => this.logs = data,
      error: (err) => console.error('error fetching log:', err)
    });
  }
  closeSidebarOnMobile(): void {
    if (window.innerWidth <= 768) {
      this.sidebarOpen = false;
    }
  }
}
