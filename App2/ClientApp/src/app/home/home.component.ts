import { Component,Inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public user: ListUser[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private router: Router) {
    http.get<ListUser[]>(baseUrl + 'loadsp').subscribe(result => {
      this.user = result;
    }, error => console.error(error));
  }
  public goView(id: string) {
    this.router.navigate(['counter', { p1: id }]);
  }
}

//khai báo class hứng data
interface ListUser {
  maSP: string;
  tenSP: string;
  tenSPKoDau: string;
  tenPhu: string;
  giamua: number;
  giasale: number;
  dateCreate: string;
  userCreate: string;
}
