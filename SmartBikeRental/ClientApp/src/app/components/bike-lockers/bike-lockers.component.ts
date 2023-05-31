import { Component, OnInit } from '@angular/core';
import { SignalRService } from 'src/app/core/services/signal-r.service';

@Component({
  selector: 'app-bike-lockers',
  templateUrl: './bike-lockers.component.html',
  styleUrls: ['./bike-lockers.component.scss'],
})
export class BikeLockersComponent implements OnInit {
  public devices$ = this.signalR.devices$;
  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {}
}
