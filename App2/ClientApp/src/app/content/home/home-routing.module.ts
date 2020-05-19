
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { DetailComponent } from './detailsp/detail.component';
import { FullListComponent } from './fullsp/full.component';
import { SlideComponent } from 'src/app/content/slidehome/slide.component';
import { HomeComponent } from './home.component';

const routesConfig: Routes = [
  //patch add defaut
  {
    path: '',
    component: HomeComponent,
    children: [{ path: 'detail', component: DetailComponent},
      { path: 'full', component: FullListComponent},
      { path: '**', component: FullListComponent}]
  }
  // 
]

@NgModule({
  declarations: [
    FullListComponent,
    DetailComponent,
    SlideComponent,
    HomeComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routesConfig),
  ],
  exports: [RouterModule]
})
export class HomeRoutingModule { }
