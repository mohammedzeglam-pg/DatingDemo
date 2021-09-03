import { HttpClient, HttpParams } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
export function getPaginatedResult<T>(
  url: string,
  params: HttpParams,
  http: HttpClient
) {
  const paginatedResult = new PaginatedResult<T>();
  return http.get<T>(url, { observe: 'response', params: params }).pipe(
    map((response) => {
      paginatedResult.result = response.body;
      if (response.headers.get('Pagination') !== null) {
        paginatedResult.pagination = JSON.parse(
          response.headers.get('Pagination')
        );
      }
      return paginatedResult;
    })
  );
}
export function getPaginationHeader(pageNumber: number, pageSize: number) {
  let params = new HttpParams();
  params = params.append('PageNumber', pageNumber.toString());
  params = params.append('PageSize', pageSize.toString());
  return params;
}
