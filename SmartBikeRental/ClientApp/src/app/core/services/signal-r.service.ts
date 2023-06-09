import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import {
  BehaviorSubject,
  Observable,
  catchError,
  filter,
  of,
  share,
  tap,
} from 'rxjs';
import { DeviceDto } from '../models/autogenerated/dtos/deviceDto';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: HubConnection;
  private apiUrl: string;

  //#region Device data
  private _devices$ = new BehaviorSubject<DeviceDto[]>(null);
  public devices$ = this._devices$.asObservable();
  //test only
  // .pipe(
  //   filter((e) => !!e),
  //   tap((ds) => {
  //     ds.forEach((el, indx) => {
  //       const maxLat = 46;
  //       const minLat = 44;
  //       const maxLng = 16;
  //       const minLng = 14;

  //       el.bikeRentalLatitude = Math.random() * (maxLat - minLat) + minLat;
  //       el.bikeRentalLongitude = Math.random() * (maxLng - minLng) + minLng;
  //       el.deviceName = `#${indx}`;
  //       el.bikeRentalLED = Math.random() >= 0.5;
  //     });
  //   }),
  //   share()
  // );

  get devices() {
    return this._devices$.value;
  }
  set devices(val: DeviceDto[]) {
    this._devices$.next(val);
  }
  //#endregion

  constructor(
    private http: HttpClient,
    @Inject('API_URL') apiUrl: string,
    @Inject('HUB_URL') private hubUrl: string
  ) {
    this.apiUrl = apiUrl + '/BikeRent';

    this.startConnection();
    this.addDataListener();
  }

  public startConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch((err) => console.log(`Error while starting connection ${err}`));
  }

  public addDataListener() {
    this.hubConnection.on('DeviceData', (data) => {
      this.devices = data;
    });
  }

  public getDataStream(): Observable<any> {
    return this.http.get(this.apiUrl).pipe(
      catchError((err) => {
        return of(null);
      })
    );
  }
}
