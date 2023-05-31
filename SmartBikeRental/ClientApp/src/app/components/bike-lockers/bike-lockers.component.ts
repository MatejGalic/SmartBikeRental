import { Component, OnInit } from '@angular/core';
import { filter, tap } from 'rxjs';
import { SignalRService } from 'src/app/core/services/signal-r.service';

@Component({
  selector: 'app-bike-lockers',
  templateUrl: './bike-lockers.component.html',
  styleUrls: ['./bike-lockers.component.scss'],
})
export class BikeLockersComponent implements OnInit {
  public devices$ = this.signalR.devices$
    //mocking data for testing
    .pipe(
      filter((e) => !!e),
      tap((ds) => {
        ds.forEach((el, indx) => {
          const maxLat = 46;
          const minLat = 44;
          const maxLng = 16;
          const minLng = 14;

          el.latitude = Math.random() * (maxLat - minLat) + minLat;
          el.longitude = Math.random() * (maxLng - minLng) + minLng;
          el.deviceName = `#${indx}`;
          el.isLocked = Math.random() >= 0.5;
        });
      })
    );
  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {}
}
