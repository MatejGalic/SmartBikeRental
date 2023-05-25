import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';

@NgModule({
  declarations: [],
  imports: [CommonModule, LeafletModule],
  exports: [LeafletModule],
})
export class SharedModule {}
