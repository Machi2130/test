import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { Report, ReportFilter } from '../Models/report';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly baseUrl = '/filter';

  constructor(private http: HttpClient) { }


  filterReports(filter: ReportFilter): Observable<Report[]> {
    return this.http.post<Report[]>(this.baseUrl, filter);
  }
}
