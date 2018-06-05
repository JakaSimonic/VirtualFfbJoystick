import { Injectable } from '@angular/core';
import { HttpClient, HttpParams  } from '@angular/common/http';
import { Settings } from './settings/settings.component'
import { Effect } from './effects/effects.component'
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class HttpService {
  constructor(private http: HttpClient) { }
  domainUrl = 'http://localhost:7331/api/';
  settingUrl = this.domainUrl + 'settings';
  effectsUrl = this.domainUrl + 'manualEffects';
  effectUrl = this.domainUrl + 'manualEffects/profile/';

  getSettings() {
    return this.http.get(this.settingUrl);
  }

  settingsSave(settings: Settings): Observable<Settings> {
    console.log(this.settingUrl);

    return this.http.post<Settings>(this.settingUrl, settings);
  }

  getProfilesList(): Observable<string[]> {
    return this.http.get<string[]>(this.effectsUrl);
  }

  getEffect(name: string): Observable<Effect> {
    return this.http.get<Effect>(this.effectUrl+'/'+name);
  }

  saveEffect(file:string, effect: Effect): Observable<any> {
    return this.http.post(this.effectUrl+file, effect);
  }
}
