import { Component } from '@angular/core';

@Component({
  selector: 'slide',
  templateUrl: './slide.component.html'
})
export class SlideComponent {
  imagesde = "https://picsum.photos/id/966/900/350";
  images = [944, 1011, 984].map((n) => `https://picsum.photos/id/${n}/900/350`);
}
