import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ExportData } from './ExportData';
@Component({
  selector: 'app-load-http',
  standalone: true,
  imports: [],
  templateUrl: './load-http.component.html',
  styleUrl: './load-http.component.css'
})
export class LoadHttpComponent implements OnInit {
  readonly urlRoot:string="http://localhost:5027/api/";
  
  @Input() Name: string = '';
  @Input() Url: string = '';
  @Input() Key: string = '';

  public value : string = 'loading...';
  @Output() getData = new EventEmitter<ExportData<string>>();
  constructor(private http: HttpClient) {

    this.value = "loading " + this.Name;
    
  }
 ngOnInit(): void {
   var urlToLoad = this.urlRoot+this.Url;
   this.http.get(urlToLoad).subscribe((data)=>{
     this.value=data.toString();
     if(this.Key != ''){
     this.getData.emit({key:this.Key,data:this.value});
    //  window.alert(this.value);
     }
   });
 }
  
}
