import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { addUser } from '../../Models/add-user';
import { AuthService } from '../../Services/auth.service';
import { LoginService } from '../../Services/login.service';
import { MatDialogRef } from '@angular/material/dialog';


@Component({
  selector: 'app-add-user',
  standalone: false,
  templateUrl: './add-user.component.html',
  styleUrl: './add-user.component.css',
})
export class AddUserComponent {
  @Output() userAdded = new EventEmitter<void>();
  registerForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';
  isAuthorized = true;
  roles = [
    { id: 1, name: 'SuperAdmin' },
    { id: 2, name: 'Supervisor' },
    { id: 3, name: 'Admin' },
    { id: 4, name: 'Guest' },
  ];

  constructor(
    private fb: FormBuilder,
    private addUserService: LoginService,
    private dialogRef: MatDialogRef<AddUserComponent>,
    private authService: AuthService
  ) {
    this.registerForm = this.fb.group({
      username: this.fb.control('', Validators.required),
      password: this.fb.control('', [
        Validators.required,
        Validators.minLength(6),
      ]),
      email: this.fb.control('', [Validators.required, Validators.email]),
      roleId: this.fb.control(4, Validators.required), // Default to Guest
    });
    if (!this.authService.hasPermission('AddUser')) {
      this.isAuthorized = false;
    }
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.registerForm.invalid) return;

    const user: addUser = this.registerForm.value;
    this.addUserService.register(user).subscribe({
      next: () => {
        this.successMessage = 'User registered successfully';
        this.errorMessage = '';
        this.registerForm.reset({ roleId: 4 });
        this.submitted = false;
        this.userAdded.emit();
      },
      error: (err) => {
        console.log('Registration error:', err);

        if (err.status === 400 && err.error?.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Registration failed. Please try again.';
        }

        this.successMessage = '';
      },
    });
  }

  onCancel() {
    this.dialogRef.close(false); // close without registering
  }
}
