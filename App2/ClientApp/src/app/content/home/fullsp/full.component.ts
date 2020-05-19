import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'full',
  templateUrl: './full.component.html',
})
export class FullListComponent {
  public user: ListUser[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private router: Router) {
    http.get<ListUser[]>(baseUrl + 'loadsp').subscribe(result => {
      this.user = result;
    }, error => console.error(error));
  }
  public goView(id: string) {
    this.router.navigate(['home/detail', { id: id }]);
  }
}

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
