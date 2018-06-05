import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { BroadcastEventListener, SignalRConnection, SignalR } from 'ng2-signalr';
import { from } from 'rxjs';
import * as Plotly from 'plotly.js';
//declare const Plotly: any;
@Component({
  selector: 'app-monitor',
  templateUrl: './monitor.component.html',
  styleUrls: ['./monitor.component.css']
})
export class MonitorComponent implements OnInit {

  maxArraySize: number = 5;
  wheel: number[] = [0, 0, 0, 0, 0];
  torque: number[] = [0, 0, 0, 0, 0];
  connection: SignalRConnection;
  @ViewChild('chart') el: ElementRef;
  constructor(private _signalR: SignalR) { }

  ngOnInit() {

    const element = this.el.nativeElement
    var trace1 = {
      y: this.wheel,
      name: 'Wheel position',
    };

    var trace2 = {
      y: this.torque,
      name: 'Torque',
    };

    var data = [trace1, trace2];

    Plotly.newPlot(element, data);

    let onMessageSent$ = new BroadcastEventListener<MonitorData>('sendToAll');

    this._signalR.connect()
      .then(c => {
        this.connection = c as SignalRConnection;
        c.listen(onMessageSent$);
      });

    onMessageSent$.subscribe((data: MonitorData) => {
      this.insertToArray(this.wheel, data.wheelPosition);
      this.insertToArray(this.torque, data.torque);
      //Plotly.purge(this.el.nativeElement);
      //Plotly.plot(element, [[{ y: this.wheel, text: "Wheel position", name: "Wheel position" }], [{ y: this.torque, text: "Torque", name: "Torque" }]] );
      //Plotly.plot(element, [{ y: this.torque, text: "Torque", name: "Torque" }], );

      var trace1 = {
        y: this.wheel,
        name: 'Wheel position',
      };

      var trace2 = {
        y: this.torque,
        name: 'Torque',
      };
      var data2 = [trace1, trace2];

      Plotly.react(this.el.nativeElement, data2);
    });
  }

  sendToAll(message: string) {
    this.connection.invoke('GetNgBeSpeakers', message).then((data: string[]) => {
      console.log(data);
    });
  }
  insertToArray(array: number[], value: number) {
    array.push(value);
    if (array.length > this.maxArraySize) {
      array.shift();
    }
  }
}

interface MonitorData {
  wheelPosition: number;
  torque: number;
}
