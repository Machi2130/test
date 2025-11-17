import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { AppLog } from '../Models/app-log.model';

@Injectable({
  providedIn: 'root'
})
export class AppLogService {
  private readonly baseUrl = '/daterange';
  private readonly applogUrl = '/allLogs';
  constructor(private http: HttpClient) { }


  getLogsByDateRange(startDate:string, endDate:string): Observable<AppLog[]>{
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return this.http.get<AppLog[]>(this.baseUrl, { params });
  }

  getAllLogs(): Observable<AppLog[]> {
    return this.http.get<AppLog[]>(this.applogUrl);
  }

  //getLogCount(): Observable<number> {
  //  return this.getAllLogs().pipe(
  //    map(logs => logs ? logs.length : 0)
  //  );
  //}
}
