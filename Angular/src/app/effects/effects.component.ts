import { Component, OnInit } from '@angular/core';
import { HttpService } from '../http.service';


@Component({
  selector: 'app-effects',
  templateUrl: './effects.component.html',
  styleUrls: ['./effects.component.css']
})
export class EffectsComponent implements OnInit {

  deadbandMin = 0;
  deadbandMax = 15;
  coefficientMin = 0;
  coefficientMax = 255;

  effectProfiles: string[];
  effect: Effect;
  selected: string;
  constructor(private http: HttpService) {
    this.effect = {
      spring: { enabled: false, coefficient: 0, deadBand: 0 },
      damper: { enabled: false, coefficient: 0, deadBand: 0 },
      inertia: { enabled: false, coefficient: 0, deadBand: 0 },
      friction: { enabled: false, coefficient: 0, deadBand: 0 },
      limit: { enabled: false, cpOfffset: 0 },
      gain: 0
    }
  }

  ngOnInit() {
    this.http.getProfilesList().subscribe(data => this.effectProfiles = data);
  }

  saveEffect() {
    this.http.saveEffect(this.selected, this.effect).subscribe(data => console.log('success', data),
      error => console.log('oops', error));
  }

  selectionChanged(event) {
    this.http.getEffect(event).subscribe(data => this.effect = data);
  }
}

export interface Effect {
  spring: ConditionEffect;
  damper: ConditionEffect;
  inertia: ConditionEffect;
  friction: ConditionEffect;
  limit: MotionLimit;
  gain: number;
}

export interface ConditionEffect {
  enabled: boolean;
  coefficient: number;
  deadBand: number;
}

export interface MotionLimit {
  enabled: boolean;
  cpOfffset: number;
}
