import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './core/login/login.component';
import { DashboardComponent } from './core/Module/dashboard/dashboard.component';
import { DashboardLayoutComponent } from './core/Module/dashboard-layout/dashboard-layout.component';
import { AuthGuard } from './core/guards/auth.guard';
import { ReportComponent } from './core/Module/report/report.component';
import { AppLogComponent } from './core/Module/app-log/app-log.component';
import { AllUserComponent } from './core/Module/all-user/all-user.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, canActivate: [AuthGuard] },

  {
    path: 'dashboard',
    canActivate: [AuthGuard],
    component: DashboardLayoutComponent, // Navbar wrapper
    children: [
      { path: '', component: DashboardComponent },
      { path: 'report', component: ReportComponent },
      { path: 'logs', component: AppLogComponent },
      { path: 'users', component: AllUserComponent }
    ]
  },

  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
