import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'detail',
  templateUrl: './detail.component.html',
})
export class DetailComponent {
  //khai bao bien
  public user: ListUser[];
  public IDC = "";
  constructor(private activatedRoute: ActivatedRoute, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private router: Router) {
    this.activatedRoute.params.subscribe(params => {
      const ID = params['id'];
      this.IDC = ID
    })
    http.get<ListUser[]>(baseUrl + 'loadsp?id=' + this.IDC).subscribe(result => {
      this.user = result;
    }, error => console.error(error));
  }
  //ngOnInit() {
  //  this.activatedRoute.params.subscribe(params => {
  //    //lấy data
  //    const ID = params['id'];
  //    this.IDC= ID
  //  })
  //}
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
