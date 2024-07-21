import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { LoadHttpComponent } from '../load-http/load-http.component';
import { ExportData } from '../load-http/ExportData';

@Component({
  selector: 'app-display-data',
  standalone: true,
  imports: [LoadHttpComponent],
  templateUrl: './display-data.component.html',
  styleUrl: './display-data.component.css'
})
export class DisplayDataComponent  {
  onGetData(data: ExportData<string>):void {
    console.log(data.key + " " + data.data);
  }
  
}
