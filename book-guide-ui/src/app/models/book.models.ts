export interface ExternalBook {
  externalBookId: string;
  title: string;
  author?: string | null;
  coverUrl?: string | null;
}
