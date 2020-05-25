import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
@Component({
  selector: 'full',
  templateUrl: './full.component.html',
})
export class FullListComponent {
  public user: ListUser[];
  public pagetotalrow: number;

  constructor(private router: Router, private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    //http.get<ListUser[]>(baseUrl + 'loadsp?pageindex=' + this.pageindex + '&pagesize=' + this.pagesize).subscribe(result => {
    //  this.user = result;
    //  this.pagetotalrow = this.user[0].totalRow;
    //  // xử lí phân trang
    //}, error => console.error(error));
    this.view();
  }
  public goView(id: string) {
    this.router.navigate(['home/detail', { id: id }]);
  }
  private solgpage: number;
  private numstar: number = 1;
  private view(pageindex: number = 1, pagesize: number = 3) {
    this.http.get<ListUser[]>(this.baseUrl + 'loadsp?pageindex=' + pageindex + '&pagesize=' + pagesize).subscribe(result => {
      this.user = result;
      this.pagetotalrow = this.user[0].totalRow;
      this.solgpage = Math.round(this.pagetotalrow / pagesize);
    }, error => console.error(error));
  }
  private numstarc() {
    if ((this.numstar + 3) < this.solgpage) {
      this.numstar = this.numstar + 1;
    }
  }
  private numstart() {
    if (this.numstar > 1) {
      this.numstar = this.numstar - 1;
    }
  }
  private cuoitrang() {
    this.view(this.solgpage);
    this.numstar = this.solgpage - 3;
  }
  private dautrang() {
    this.view(1);
    this.numstar = 1;
  }
}

interface ListUser {
  totalRow: number;
  maSP: string;
  tenSP: string;
  tenSPKoDau: string;
  tenPhu: string;
  giamua: number;
  giasale: number;
  dateCreate: string;
  userCreate: string;
}
