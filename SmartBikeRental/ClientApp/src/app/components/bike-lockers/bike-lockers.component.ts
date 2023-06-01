import { Component } from '@angular/core';
import { merge, take, tap } from 'rxjs';
import { BikeRentService } from 'src/app/core/services/bike-rent.service';
import { SignalRService } from 'src/app/core/services/signal-r.service';

@Component({
  selector: 'app-bike-lockers',
  templateUrl: './bike-lockers.component.html',
  styleUrls: ['./bike-lockers.component.scss'],
})
export class BikeLockersComponent {
  public devices$ = merge(
    this.bikeRentService.getDevices().pipe(take(1)),
    this.signalR.devices$
  );

  constructor(
    private signalR: SignalRService,
    private bikeRentService: BikeRentService
  ) {}
}
