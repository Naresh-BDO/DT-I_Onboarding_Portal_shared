import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateNewJoinerDto {
  fullName: string;
  email: string;
  department?: string;
  managerName?: string;
  startDate: string; // ISO string
}

@Injectable({ providedIn: 'root' })
export class NewJoinerService {
  private apiUrl = 'https://localhost:7107/api/new-joiners';

  constructor(private http: HttpClient) {}

  createNewJoiner(dto: CreateNewJoinerDto): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }
}