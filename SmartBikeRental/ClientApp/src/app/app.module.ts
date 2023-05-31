import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BikeRentMapComponent } from './components/bike-rent-map/bike-rent-map.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { SharedModule } from './core/modules/shared.module';
import { HomeComponent } from './pages/home/home.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { BikeLockersComponent } from './components/bike-lockers/bike-lockers.component';
import { BikeLockerComponent } from './components/bike-locker/bike-locker.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    NotFoundComponent,
    BikeRentMapComponent,
    BikeLockersComponent,
    BikeLockerComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    SharedModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
