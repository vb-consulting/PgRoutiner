DO $zwatts_schema$
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
ALTER TABLE IF EXISTS ONLY public.users_devices DROP CONSTRAINT IF EXISTS "FK_users_devices_user_id";
ALTER TABLE IF EXISTS ONLY public.users_devices DROP CONSTRAINT IF EXISTS "FK_users_devices_device";
ALTER TABLE IF EXISTS ONLY public.power DROP CONSTRAINT IF EXISTS "FK_energy_device";
ALTER TABLE IF EXISTS ONLY public.energy DROP CONSTRAINT IF EXISTS "FK_energy_device";
DROP INDEX IF EXISTS public."IDX_users_email_normalized";
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pkey;
ALTER TABLE IF EXISTS ONLY public.devices DROP CONSTRAINT IF EXISTS devices_pkey;
ALTER TABLE IF EXISTS ONLY public.users_devices DROP CONSTRAINT IF EXISTS "PK_users_devices";
ALTER TABLE IF EXISTS ONLY public.power DROP CONSTRAINT IF EXISTS "PK_power";
ALTER TABLE IF EXISTS ONLY public.energy DROP CONSTRAINT IF EXISTS "PK_energy";
DROP TABLE IF EXISTS public.users_devices;
DROP TABLE IF EXISTS public.users;
DROP TABLE IF EXISTS public.testclass;
DROP TABLE IF EXISTS public.power;
DROP TABLE IF EXISTS public.energy;
DROP TABLE IF EXISTS public.devices;
DROP FUNCTION IF EXISTS public.upsert_power_values(_address character varying, _data json);
DROP FUNCTION IF EXISTS public.upsert_energy_values(_address character varying, _data json);
DROP FUNCTION IF EXISTS public.remove_values(_start bigint, _end bigint);
DROP FUNCTION IF EXISTS public.remove_power_values(_start bigint, _end bigint);
DROP FUNCTION IF EXISTS public.remove_all_values(_start bigint, _end bigint);
DROP FUNCTION IF EXISTS public.device_login(_address character varying, _pin character varying, _email character varying);
--
-- Name: device_login(character varying, character varying, character varying); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.device_login(_address character varying, _pin character varying, _email character varying) RETURNS bigint
    LANGUAGE plpgsql SECURITY DEFINER
    AS $$
declare _user_id bigint;
declare _email_normalized varchar;
declare _mac macaddr;
begin
    _mac := _address::macaddr;
    _email_normalized := lower(_email);
    _user_id := (select id from users where email_normalized = _email_normalized);
        
    if (_user_id is null) then
        insert into users (email, email_normalized)
        values (_email, _email_normalized)
        on conflict do nothing
        returning id into _user_id;
    end if;
    
    insert into devices (address, pin)
    values (_mac, _pin)
    on conflict do nothing;
    
    insert into users_devices (device, user_id)
    values (_mac, _user_id)
    on conflict do nothing;
        
    return _user_id;
end    
$$;
--
-- Name: remove_all_values(bigint, bigint); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.remove_all_values(_start bigint, _end bigint) RETURNS void
    LANGUAGE sql SECURITY DEFINER
    AS $$
    select remove_values(_start, _end);
    select remove_power_values(_start, _end);
$$;
--
-- Name: remove_power_values(bigint, bigint); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.remove_power_values(_start bigint, _end bigint) RETURNS void
    LANGUAGE sql SECURITY DEFINER
    AS $$
delete from power 
where time between to_timestamp(_start::double precision / 1000)::timestamp and to_timestamp(_end::double precision / 1000)::timestamp;
$$;
--
-- Name: remove_values(bigint, bigint); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.remove_values(_start bigint, _end bigint) RETURNS void
    LANGUAGE sql SECURITY DEFINER
    AS $$
delete from power 
where time between to_timestamp(_start::double precision / 1000)::timestamp and to_timestamp(_end::double precision / 1000)::timestamp;
delete from energy 
where time between to_timestamp(_start::double precision / 1000)::timestamp and to_timestamp(_end::double precision / 1000)::timestamp;
$$;
--
-- Name: upsert_energy_values(character varying, json); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.upsert_energy_values(_address character varying, _data json) RETURNS bigint
    LANGUAGE sql SECURITY DEFINER
    AS $$
with cte as (
    insert into energy (time, value, device, type)
    select 
        distinct
        to_timestamp(time::double precision / 1000)::timestamp as time, 
        value, 
        _address::macaddr as device,
        type
    from (
        select time, max(value) as value, type 
        from json_to_recordset(_data) as data(time bigint, value bigint, type int)
        group by time, type 
    ) as sub
    on conflict on constraint "PK_energy" 
    do update set value = EXCLUDED.value
    returning *
)
select count(*) from cte
$$;
--
-- Name: upsert_power_values(character varying, json); Type: FUNCTION; Schema: public; Owner: -
--
CREATE FUNCTION public.upsert_power_values(_address character varying, _data json) RETURNS bigint
    LANGUAGE sql SECURITY DEFINER
    AS $$
with cte as (
    insert into power (time, value, device, type)
    select 
        to_timestamp(time::double precision / 1000)::timestamp as time, 
        value, 
        _address::macaddr as device,
        type
    from json_to_recordset(_data) as data(time bigint, value double precision, type int)
    on conflict on constraint "PK_power" 
    do update set value = EXCLUDED.value
    returning *
)
select count(*) from cte
$$;
SET default_tablespace = '';
SET default_table_access_method = heap;
--
-- Name: devices; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.devices (
    address macaddr NOT NULL,
    pin character varying NOT NULL,
    "timestamp" timestamp without time zone DEFAULT now() NOT NULL
);
--
-- Name: energy; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.energy (
    "time" timestamp without time zone NOT NULL,
    value bigint NOT NULL,
    device macaddr NOT NULL,
    type integer NOT NULL,
    CONSTRAINT energy_type_check CHECK (((type = 0) OR (type = 181) OR (type = 182) OR (type = 280) OR (type = 380) OR (type = 480)))
);
--
-- Name: power; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.power (
    "time" timestamp without time zone NOT NULL,
    value double precision NOT NULL,
    device macaddr NOT NULL,
    type integer
);
--
-- Name: testclass; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.testclass (
    id integer,
    foo text,
    day date,
    bool boolean,
    bar text
);
--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.users (
    id bigint NOT NULL,
    email character varying(256) NOT NULL,
    email_normalized character varying(256) NOT NULL
);
--
-- Name: users_devices; Type: TABLE; Schema: public; Owner: -
--
CREATE TABLE public.users_devices (
    device macaddr NOT NULL,
    user_id bigint NOT NULL
);
--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--
ALTER TABLE public.users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
--
-- Name: energy PK_energy; Type: CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.energy
    ADD CONSTRAINT "PK_energy" PRIMARY KEY ("time", device, type);
--
-- Name: power PK_power; Type: CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.power
    ADD CONSTRAINT "PK_power" PRIMARY KEY ("time", device);
--
-- Name: users_devices PK_users_devices; Type: CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.users_devices
    ADD CONSTRAINT "PK_users_devices" PRIMARY KEY (device, user_id);
--
-- Name: devices devices_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.devices
    ADD CONSTRAINT devices_pkey PRIMARY KEY (address);
--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);
--
-- Name: IDX_users_email_normalized; Type: INDEX; Schema: public; Owner: -
--
CREATE UNIQUE INDEX "IDX_users_email_normalized" ON public.users USING btree (email_normalized);
--
-- Name: energy FK_energy_device; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.energy
    ADD CONSTRAINT "FK_energy_device" FOREIGN KEY (device) REFERENCES public.devices(address) DEFERRABLE;
--
-- Name: power FK_energy_device; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.power
    ADD CONSTRAINT "FK_energy_device" FOREIGN KEY (device) REFERENCES public.devices(address) DEFERRABLE;
--
-- Name: users_devices FK_users_devices_device; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.users_devices
    ADD CONSTRAINT "FK_users_devices_device" FOREIGN KEY (device) REFERENCES public.devices(address) ON DELETE CASCADE DEFERRABLE;
--
-- Name: users_devices FK_users_devices_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.users_devices
    ADD CONSTRAINT "FK_users_devices_user_id" FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE DEFERRABLE;
--
-- PostgreSQL database dump complete
--
END $zwatts_schema$
LANGUAGE plpgsql;
