import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import { DeviceDto } from '../models/autogenerated/dtos/deviceDto';

@Injectable({
  providedIn: 'root',
})
export class BikeRentService {
  private apiUrl: string;

  constructor(private http: HttpClient, @Inject('API_URL') apiUrl: string) {
    this.apiUrl = apiUrl + '/BikeRent';
  }

  //TODO: finish method
  public unlockDevice(deviceId: string): Observable<any> {
    return this.http.post<boolean>(this.apiUrl, deviceId).pipe(
      catchError((err) => of(null)),
      map((e) => !!e)
    );
  }

  //TODO: finish method
  public getDevices(): Observable<DeviceDto[]> {
    return this.http
      .get<DeviceDto[]>(this.apiUrl)
      .pipe(catchError((err) => of([])));
  }
}
