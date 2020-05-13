import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { FooterComponent } from './footer/footer.component';
import { SlideComponent } from './slidehome/slide.component';

const routesConfig: Routes = [
//patch add defaut
 
  { path: 'counter', component: CounterComponent },
  { path: 'fetch-data', component: FetchDataComponent },
  { path: '**', component: HomeComponent }
]

@NgModule({
  declarations: [
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    FooterComponent,
    SlideComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forRoot(routesConfig)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
