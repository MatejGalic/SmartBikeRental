import { Component, OnDestroy, OnInit } from '@angular/core';
import * as L from 'leaflet';
import { Subject, of, switchMap, takeUntil } from 'rxjs';
import { SignalRService } from 'src/app/core/services/signal-r.service';

@Component({
  selector: 'app-bike-rent-map',
  templateUrl: './bike-rent-map.component.html',
  styleUrls: ['./bike-rent-map.component.scss'],
})
export class BikeRentMapComponent implements OnInit, OnDestroy {
  private readonly LAT_DEFAULT = 45.8153;
  private readonly LNG_DEFAULT = 15.9665;
  private map: L.Map;
  private popups: L.Popup[] = [];
  private destroy$: Subject<void> = new Subject<void>();

  private testData: DeviceData[] = [
    new DeviceData(),
    new DeviceData(),
    new DeviceData(),
    new DeviceData(),
  ];

  constructor(private signalR: SignalRService) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    this.signalR.devices$
      .pipe(
        takeUntil(this.destroy$),
        switchMap((e) => of(this.generateRandomDeviceData()))
      )
      .subscribe((devices) => {
        // update device info in popup
        this.popups.forEach((p, i) => {
          const device = devices[i];
          p.setContent(`<h1>Device #${i}</h1>
          ${device.lat} <br>
          Is locked: ${device.locked}`);
          p.setLatLng([device.lat, device.lng]);
        });
      });
  }

  // used for testing
  private generateRandomDeviceData(): DeviceData[] {
    const maxLat = 46;
    const minLat = 44;
    const maxLng = 16;
    const minLng = 14;

    this.testData.forEach((el, indx) => {
      el.lat = Math.random() * (maxLat - minLat) + minLat;
      el.lng = Math.random() * (maxLng - minLng) + minLng;
      el.name = `#${indx}`;
      el.locked = Math.random() >= 0.5;
    });

    return this.testData;
  }

  public options: L.MapOptions = {
    layers: [
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        minZoom: 1,
        attribution:
          'Map data © <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
      }),
    ],
    zoom: 7,
    center: L.latLng(this.LAT_DEFAULT, this.LNG_DEFAULT),
  };

  public onMapReady(map: L.Map) {
    this.map = map;

    // set initial device data on map
    this.testData.forEach((t) => {
      const p = L.popup({
        closeButton: false,
        autoClose: false,
        closeOnClick: false,
        closeOnEscapeKey: false,
      })
        .setLatLng([this.LAT_DEFAULT + 1, this.LNG_DEFAULT + 1])
        .setContent('loading data')
        .addTo(this.map)
        .openPopup();

      this.popups.push(p);
    });
  }
}

interface IDeviceData {
  name: string;
  lat: number;
  lng: number;
  locked: boolean;
}
class DeviceData implements IDeviceData {
  name: string;
  lat: number;
  lng: number;
  locked: boolean;
}
