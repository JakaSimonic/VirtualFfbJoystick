import { Component, OnInit } from '@angular/core';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  settings:Settings;
  constructor(private configService: HttpService)
  {
    this.settings={plc:{ ipaddress:"waiting for server...",pollPeriod:500,port:0},driver:{pollPeriod:500}};
  }
  ngOnInit(): void {
    this.showConfig();
    }

    showConfig(): void {
    this.configService.getSettings().
    subscribe((data) => this.settings = data as Settings);
  }

  onSave():void{
    this.configService.settingsSave(this.settings).subscribe();
  }
}


export interface Plc
{
    ipaddress: string ;
    port: number;
    pollPeriod:number;
}

export interface Driver
{
    pollPeriod:number;
}
export interface Settings
{
    plc:Plc ;
    driver: Driver ;
}
