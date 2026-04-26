export interface PageDTO<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};
