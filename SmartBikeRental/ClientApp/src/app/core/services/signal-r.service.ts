import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: HubConnection;
  private apiUrl: string;

  //#region Device data
  private _devices$ = new BehaviorSubject<any>(null);
  public devices$ = this._devices$.asObservable();

  get devices() {
    return this._devices$.value;
  }
  set devices(val: any) {
    this._devices$.next(val);
  }
  //#endregion

  constructor(
    private http: HttpClient,
    @Inject('API_URL') apiUrl: string,
    @Inject('HUB_URL') private hubUrl: string
  ) {
    this.apiUrl = apiUrl + '/BikeRent';
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
      // this.data = data;
      console.log(data);
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
