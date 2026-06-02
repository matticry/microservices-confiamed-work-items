create table tbl_user
(
    id_us       int identity
        constraint tbl_user_pk
            primary key,
    username_us varchar(max),
    status_us   char,
    create_at   datetime2
)
    go

create table tbl_work_items
(
    id_wi           int identity
        constraint tbl_work_items_pk
            primary key,
    code_wi         varchar(max),
    description_wi  varchar(max),
    status_wi       char,
    relevance       char,
    created_at      datetime2,
    expiration_date datetime2
)
    go

create table tbl_user_work
(
    id_u_w          int identity
        constraint tbl_user_work_pk
            primary key,
    user_id         int
        constraint tbl_user_work_tbl_user_id_us_fk
            references tbl_user,
    item_id         int
        constraint tbl_user_work_tbl_work_items_id_wi_fk
            references tbl_work_items,
    status          char,
    assignment_date datetime2,
    completion_date datetime2,
    order_priority  int
)
    go

