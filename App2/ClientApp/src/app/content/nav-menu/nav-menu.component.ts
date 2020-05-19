import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NavModel } from 'src/app/model/nav.class';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  public navmenu: NavModel[];
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<NavModel[]>(baseUrl + 'nav').subscribe(result => {
      this.navmenu = result;
    }, error => console.error(error));
  }

  isExpanded = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
