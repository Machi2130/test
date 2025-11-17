import { Component, OnInit } from '@angular/core';
import { User } from '../../Models/user.model';
import { LoginService } from '../../Services/login.service';

@Component({
  selector: 'app-all-user',
  standalone: false,
  templateUrl: './all-user.component.html',
  styleUrl: './all-user.component.css'
})
export class AllUserComponent implements OnInit {

  users: User[] = [];
  loading = false;
  errorMessage: string | null = null;

  constructor(private loginService: LoginService) { }

  ngOnInit() {
    this.fetchUsers();
  }

  fetchUsers(): void {
    this.loading = true;
    this.loginService.getAllUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to Load Users';
        this.loading = false;
      }
    });
  }
}
