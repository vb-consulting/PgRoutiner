DO $zwatts_data$
BEGIN
--
-- PostgreSQL database dump
--
-- Dumped from database version 12.0
-- Dumped by pg_dump version 12.0
SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
PERFORM pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;
--
-- Data for Name: testclass; Type: TABLE DATA; Schema: public; Owner: postgres
--
INSERT INTO public.testclass VALUES (1, 'foo1', '1977-05-19', true, NULL);
INSERT INTO public.testclass VALUES (2, 'foo2', '1978-05-19', false, 'bar2');
INSERT INTO public.testclass VALUES (3, 'foo3', '1979-05-19', NULL, 'bar3');
--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--
INSERT INTO public.users OVERRIDING SYSTEM VALUE VALUES (1, 'vbilopa@gmail.com', 'vbilopa@gmail.com');
--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--
PERFORM pg_catalog.setval('public.users_id_seq', 1, true);
--
-- PostgreSQL database dump complete
--
END $zwatts_data$
LANGUAGE plpgsql;
