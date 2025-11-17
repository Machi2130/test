import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { AddUserComponent } from '../add-user/add-user.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-dashboard-layout',
  standalone: false,
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.css'
})
export class DashboardLayoutComponent {

  username: string = '';
  dropdownOpen = false;
  showAddUserForm = false; // <-- Add this
  activeSection: 'reports' | 'logs' | null = null;
  canAddUser = false;
  canViewReports = false;



  constructor(private auth: AuthService, private router: Router, private dialog: MatDialog) { }

  ngOnInit(): void {
    const user = this.auth.getUser();
    if (user) {
      this.username = user.username;
      this.canAddUser = this.auth.hasPermission('AddUser');
      this.canViewReports = this.auth.hasPermission('ViewReport');
    } else {
      this.router.navigate(['/login']); 
    }
  }

  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
  }
  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  addUser() {
    console.log("Open Add User form");
    this.showAddUserForm = true;
    const dialogRef = this.dialog.open(AddUserComponent, {
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('User successfully added!');
      }
    });
  }

  onUserAdded() {
    this.showAddUserForm = false; // <-- Hide after user is added
  }
  // showReports() {
  //   this.activeSection = 'reports';
  // }

  // showLogs() {
  //   this.activeSection = 'logs';
  // }
}
